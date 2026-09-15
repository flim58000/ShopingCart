"use client";

import { createContext, useCallback, useContext, useEffect, useRef, useState } from "react";
import { api, ApiError } from "../lib/api";
import type { Cart, Order, Product } from "../lib/types";

const CART_KEY = "shopingcart:cart-id";
const ORDER_KEY = "shopingcart:last-order";

interface ShopContextValue {
  products: Product[];
  cart: Cart | null;
  order: Order | null;
  loading: boolean;
  busy: boolean;
  error: string;
  notice: string;
  refresh: () => Promise<void>;
  add: (productId: number) => Promise<void>;
  update: (productId: number, quantity: number) => Promise<void>;
  remove: (productId: number) => Promise<void>;
  clear: () => Promise<void>;
  checkout: () => Promise<void>;
}

const ShopContext = createContext<ShopContextValue | null>(null);

export function ShopProvider({ children }: { children: React.ReactNode }) {
  const [products, setProducts] = useState<Product[]>([]);
  const [cart, setCart] = useState<Cart | null>(null);
  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState("");
  const [notice, setNotice] = useState("");
  const pending = useRef(false);

  const saveOrder = useCallback((completedOrder: Order) => {
    setOrder(completedOrder);
    sessionStorage.setItem(ORDER_KEY, JSON.stringify(completedOrder));
    localStorage.removeItem(CART_KEY);
    setCart(null);
  }, []);

  const load = useCallback(async () => {
    const availableProducts = await api.products();
    setProducts(availableProducts);
    const savedId = localStorage.getItem(CART_KEY);
    if (!savedId) { setCart(null); return; }
    try {
      const savedCart = await api.cart(savedId);
      if (savedCart.status === "CheckedOut") {
        // กู้ผลลัพธ์ได้เมื่อชำระเงินสำเร็จ แต่ request ก่อนหน้าหลุดหรือผู้ซื้อรีเฟรช
        saveOrder(await api.checkout(savedId, 0));
      } else {
        setCart(savedCart);
      }
    } catch (error) {
      if (error instanceof ApiError && (error.status === 404 || error.status === 400)) {
        localStorage.removeItem(CART_KEY);
        setCart(null);
      } else throw error;
    }
  }, [saveOrder]);

  // ทุกปุ่มใช้ทางเดียวกัน: รอ API → อัปเดตหน้าจอ → ปลดปุ่ม
  const run = useCallback(async (action: () => Promise<void>) => {
    if (pending.current) return;
    pending.current = true;
    setBusy(true);
    setError("");
    setNotice("");
    try {
      await action();
    } catch (error) {
      setError(error instanceof Error ? error.message : "เกิดข้อผิดพลาด กรุณาลองใหม่");
      // โหลดสต็อก/ราคาใหม่ แต่เก็บข้อความผิดพลาดไว้ให้ผู้ซื้ออ่าน
      await load().catch(() => {});
    } finally {
      pending.current = false;
      setBusy(false);
      setLoading(false);
    }
  }, [load]);

  useEffect(() => {
    try {
      const savedOrder = sessionStorage.getItem(ORDER_KEY);
      if (savedOrder) setOrder(JSON.parse(savedOrder));
    } catch { sessionStorage.removeItem(ORDER_KEY); }
    void run(load);
    const reload = () => { void run(load); };
    window.addEventListener("focus", reload);
    window.addEventListener("storage", reload);
    return () => {
      window.removeEventListener("focus", reload);
      window.removeEventListener("storage", reload);
    };
  }, [load, run]);

  const value: ShopContextValue = {
    products, cart, order, loading, busy, error, notice,
    refresh: () => run(load),
    add: (productId) => run(async () => {
      let currentCart = cart;
      if (!currentCart) {
        currentCart = await api.createCart();
        localStorage.setItem(CART_KEY, currentCart.id);
        setCart(currentCart);
      }
      setCart(await api.add(currentCart.id, productId));
      setOrder(null);
      sessionStorage.removeItem(ORDER_KEY);
      setNotice("เพิ่มสินค้าในตะกร้าแล้ว");
    }),
    update: (productId, quantity) => run(async () => {
      if (cart) setCart(await api.update(cart.id, productId, quantity));
    }),
    remove: (productId) => run(async () => {
      if (cart) setCart(await api.remove(cart.id, productId));
      setNotice("ลบสินค้าออกจากตะกร้าแล้ว");
    }),
    clear: () => run(async () => {
      if (cart) setCart(await api.clear(cart.id));
      setNotice("ล้างตะกร้าเรียบร้อยแล้ว");
    }),
    checkout: () => run(async () => {
      if (!cart?.items.length) return;
      const completedOrder = await api.checkout(cart.id, cart.totalSatang);
      saveOrder(completedOrder);
      setProducts(await api.products());
      setNotice("ชำระเงินสำเร็จ ขอบคุณที่เลือกซื้อกับเรา");
    }),
  };

  return <ShopContext.Provider value={value}>{children}</ShopContext.Provider>;
}

export function useShop() {
  const shop = useContext(ShopContext);
  if (!shop) throw new Error("useShop must be used inside ShopProvider");
  return shop;
}
