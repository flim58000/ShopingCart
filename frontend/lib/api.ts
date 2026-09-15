import type { Cart, Order, Product } from "./types";

const baseUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5056";

export class ApiError extends Error {
  constructor(message: string, public status: number) { super(message); }
}

async function request<T>(path: string, method = "GET", body?: unknown): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`${baseUrl}${path}`, {
      method,
      headers: body === undefined ? undefined : { "Content-Type": "application/json" },
      body: body === undefined ? undefined : JSON.stringify(body),
      cache: "no-store",
    });
  } catch {
    throw new Error("เชื่อมต่อระบบไม่ได้ กรุณาตรวจสอบว่า API เปิดอยู่ แล้วลองอีกครั้ง");
  }
  if (!response.ok) {
    const problem = await response.json().catch(() => null);
    throw new ApiError(problem?.title ?? "ไม่สามารถทำรายการได้ กรุณาลองอีกครั้ง", response.status);
  }
  return response.json();
}

export const api = {
  products: () => request<Product[]>("/api/products"),
  createCart: () => request<Cart>("/api/carts", "POST"),
  cart: (id: string) => request<Cart>(`/api/carts/${id}`),
  add: (id: string, productId: number) => request<Cart>(`/api/carts/${id}/items`, "POST", { productId, quantity: 1 }),
  update: (id: string, productId: number, quantity: number) =>
    request<Cart>(`/api/carts/${id}/items/${productId}`, "PUT", { quantity }),
  remove: (id: string, productId: number) => request<Cart>(`/api/carts/${id}/items/${productId}`, "DELETE"),
  clear: (id: string) => request<Cart>(`/api/carts/${id}/items`, "DELETE"),
  checkout: (id: string, expectedTotalSatang: number) =>
    request<Order>(`/api/carts/${id}/checkout`, "POST", { expectedTotalSatang }),
};

export function money(satang: number): string {
  return new Intl.NumberFormat("th-TH", { style: "currency", currency: "THB" }).format(satang / 100);
}
