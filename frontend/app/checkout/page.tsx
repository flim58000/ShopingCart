"use client";

import Link from "next/link";
import { useShop } from "../../components/ShopProvider";
import { CartSummary } from "../../components/CartSummary";
import { money } from "../../lib/api";

export default function CheckoutPage() {
  const { cart, order, loading } = useShop();
  if (loading) return <div className="empty-state" role="status">กำลังโหลดรายการ…</div>;
  if (order && !cart?.items.length) return <section className="receipt">
    <div className="success-mark" aria-hidden="true">✓</div><span className="eyebrow">THANK YOU FOR YOUR ORDER</span><h1>ชำระเงินสำเร็จ</h1><p>ขอบคุณที่ให้ everyday เป็นส่วนหนึ่งในวันของคุณ</p>
    <div className="receipt-meta"><span>คำสั่งซื้อ #{String(order.id).padStart(6, "0")}</span><span>{new Date(order.createdAt).toLocaleString("th-TH")}</span></div>
    {order.items.map(item => <div className="receipt-row" key={item.productId}><span>{item.productName} × {item.quantity}</span><strong>{money(item.lineTotalSatang)}</strong></div>)}
    <div className="summary-total"><span>ยอดชำระทั้งหมด</span><strong>{money(order.totalSatang)}</strong></div>
    <p className="payment-note">ชำระเงินจำลองสำเร็จ ระบบปรับสต็อกสินค้าแล้ว</p><Link href="/" className="button primary full">กลับไปเลือกสินค้า <span aria-hidden="true">↗</span></Link>
  </section>;
  return <>
    <div className="page-heading"><Link href="/cart" className="back-link">← กลับไปแก้ไขตะกร้า</Link><span className="eyebrow">ONE LAST STEP</span><h1>ตรวจสอบและชำระเงิน</h1><p>อีกขั้นตอนเดียว ก็พร้อมสำหรับวันทำงานที่ดีขึ้น</p></div>
    <div className="shopping-layout"><section className="checkout-panel"><h2>รายการที่คุณเลือก</h2>
      {!cart?.items.length ? <div className="empty-state"><h2>ยังไม่มีสินค้าสำหรับชำระเงิน</h2><Link className="button secondary" href="/">เลือกซื้อสินค้า ↗</Link></div> : cart.items.map(item =>
        <div className="checkout-item" key={item.productId}><div><span className="eyebrow">{item.productCode}</span><h3>{item.productName}</h3><p>{money(item.unitPriceSatang)} × {item.quantity} ชิ้น</p></div><strong>{money(item.lineTotalSatang)}</strong></div>)}
      <div className="payment-method"><span className="payment-radio" aria-hidden="true" /><div><strong>ชำระเงินจำลอง</strong><p>กดยืนยันเพื่อทดลองสั่งซื้อ โดยไม่มีการเรียกเก็บเงินจริง</p></div><span className="demo-badge">DEMO</span></div>
    </section><CartSummary checkout /></div>
  </>;
}
