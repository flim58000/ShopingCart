"use client";

import Link from "next/link";
import { useShop } from "../../components/ShopProvider";
import { ProductArt } from "../../components/ProductArt";
import { CartSummary } from "../../components/CartSummary";
import { money } from "../../lib/api";

export default function CartPage() {
  const { cart, loading, busy, update, remove, clear } = useShop();
  const items = cart?.items ?? [];
  return <>
    <div className="page-heading"><Link className="back-link" href="/">← เลือกสินค้าต่อ</Link><span className="eyebrow">YOUR EVERYDAY PICKS</span><h1>ตะกร้าของฉัน<span className="heading-count">{cart?.totalQuantity ?? 0}</span></h1><p>ทบทวนของที่เลือก แล้วเตรียมพร้อมสำหรับวันดี ๆ</p></div>
    {loading ? <div className="empty-state" role="status">กำลังโหลดตะกร้า…</div> :
    <div className="shopping-layout"><section className="cart-panel" aria-label="สินค้าในตะกร้า">
      <div className="cart-panel-heading"><h2>รายการสินค้า <span>({items.length})</span></h2><button className="text-button danger" disabled={busy || !items.length} onClick={() => void clear()}>ล้างตะกร้า</button></div>
      {!items.length ? <div className="empty-state"><span className="empty-symbol" aria-hidden="true">＋</span><h2>ยังไม่มีสินค้าในตะกร้า</h2><p>เริ่มจากของชิ้นเล็ก ๆ ที่ทำให้วันของคุณดีขึ้น</p><Link className="button primary" href="/">เลือกซื้อสินค้า ↗</Link></div> : items.map(item =>
        <article className="cart-item" key={item.productId}>
          <div className={`cart-art tone-${item.productCode}`}><ProductArt code={item.productCode} /></div>
          <div className="cart-item-info"><span className="eyebrow">{item.productCode}</span><h3>{item.productName}</h3><p>{money(item.unitPriceSatang)} / ชิ้น</p>
            {item.quantity > item.stockQuantity && <p className="stock-warning">เหลือ {item.stockQuantity} ชิ้น กรุณาลดจำนวน</p>}
            <button className="text-button remove-button" disabled={busy} onClick={() => void remove(item.productId)} aria-label={`ลบ ${item.productName} ออกจากตะกร้า`}>ลบสินค้า</button>
          </div>
          <div className="quantity"><button disabled={busy} onClick={() => void update(item.productId, item.quantity - 1)} aria-label={`ลดจำนวน ${item.productName}`}>−</button><span aria-label={`จำนวน ${item.productName}`}>{item.quantity}</span><button disabled={busy || item.quantity >= item.stockQuantity} onClick={() => void update(item.productId, item.quantity + 1)} aria-label={`เพิ่มจำนวน ${item.productName}`}>+</button></div>
          <strong className="line-total">{money(item.lineTotalSatang)}</strong>
        </article>)}
    </section><CartSummary /></div>}
  </>;
}
