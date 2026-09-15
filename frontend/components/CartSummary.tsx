"use client";

import Link from "next/link";
import { money } from "../lib/api";
import { useShop } from "./ShopProvider";

export function CartSummary({ checkout = false }: { checkout?: boolean }) {
  const { cart, busy, loading, checkout: pay } = useShop();
  const items = cart?.items ?? [];
  const shortage = items.some(item => item.quantity > item.stockQuantity);
  const disabled = loading || busy || items.length === 0 || shortage;
  return <aside className="summary">
    <span className="eyebrow">YOUR SELECTION</span>
    <h2>สรุปคำสั่งซื้อ</h2>
    <div className="summary-row"><span>สินค้าทั้งหมด</span><span>{cart?.totalQuantity ?? 0} ชิ้น</span></div>
    <div className="summary-row"><span>ยอดสินค้า</span><span>{money(cart?.totalSatang ?? 0)}</span></div>
    <div className="summary-total"><span>ยอดรวม</span><strong>{money(cart?.totalSatang ?? 0)}</strong></div>
    {shortage && <p className="stock-warning">สินค้าไม่เพียงพอ กรุณาปรับจำนวนในตะกร้า</p>}
    {checkout ? <button className="button primary full" disabled={disabled} onClick={() => void pay()}>
      {busy ? "กำลังทำรายการ…" : "ยืนยันชำระเงิน"}<span aria-hidden="true">↗</span>
    </button> : disabled ? <button className="button primary full" disabled>ไปชำระเงิน <span aria-hidden="true">↗</span></button> :
      <Link href="/checkout" className="button primary full">ไปชำระเงิน <span aria-hidden="true">↗</span></Link>}
    <p className="summary-note">{checkout ? "การชำระเงินจำลองสำหรับทดลองใช้งาน" : "ตรวจสอบรายการและยอดรวมอีกครั้งก่อนชำระเงิน"}</p>
    <div className="summary-footer"><span aria-hidden="true">✓</span> สต็อกอัปเดตเมื่อสั่งซื้อสำเร็จ</div>
  </aside>;
}
