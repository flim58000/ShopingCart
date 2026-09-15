"use client";

import Link from "next/link";
import { useShop } from "../components/ShopProvider";
import { ProductArt } from "../components/ProductArt";
import { money } from "../lib/api";

const descriptions: Record<string, string> = {
  P001: "คล่องตัวทุกการคลิก", P002: "คู่ใจทุกไอเดีย", P003: "โฟกัสในแบบของคุณ", P004: "พื้นที่สำหรับความคิดใหม่",
};

export default function ProductsPage() {
  const { products, cart, loading, busy, error, add, refresh } = useShop();
  return <>
    <section className="hero">
      <div><span className="eyebrow">THE EVERYDAY COLLECTION / 01</span><h1>จัดโต๊ะใหม่<br /><span>ให้วันทำงานดีขึ้น</span></h1><p>อุปกรณ์ชิ้นโปรดที่เรียบง่าย ใช้ได้ทุกวัน<br />เลือกสิ่งที่ใช่ แล้วเริ่มวันดี ๆ ไปด้วยกัน</p></div>
      <div className="hero-note"><span className="hero-symbol" aria-hidden="true">✳</span><p>Small things.<br />Better days.</p><span className="eyebrow">MADE FOR YOUR EVERYDAY</span></div>
    </section>
    <section aria-labelledby="products-title">
      <div className="section-heading"><div><h2 id="products-title">เลือกของเข้ามุมทำงาน</h2><p>{products.length} สินค้าที่คัดมาให้คุณ</p></div><button className="text-button" disabled={busy || loading} onClick={() => void refresh()}>อัปเดตสต็อก <span aria-hidden="true">↻</span></button></div>
      {loading ? <div className="empty-state" role="status">กำลังโหลดสินค้า…</div> : error && products.length === 0 ?
        <div className="empty-state">ยังโหลดสินค้าไม่ได้ กด “ลองใหม่” ด้านบนเพื่อเชื่อมต่ออีกครั้ง</div> :
        <div className="product-grid">{products.map(product => {
          const inCart = cart?.items.find(item => item.productId === product.id)?.quantity ?? 0;
          const soldOut = product.stockQuantity === 0;
          const limit = inCart >= product.stockQuantity;
          return <article className="product-card" key={product.id}>
            <div className={`product-picture tone-${product.code}`}><span className="product-code">{product.code}</span>
              <span className={`stock-badge ${soldOut ? "sold-out" : ""}`}>{soldOut ? "สินค้าหมด" : `เหลือ ${product.stockQuantity} ชิ้น`}</span>
              <ProductArt code={product.code} />
            </div>
            <div className="product-info"><div><h3>{product.name}</h3><p>{descriptions[product.code] ?? "ของใช้สำหรับทุกวัน"}</p></div><strong>{money(product.priceSatang)}</strong></div>
            <button className="button product-add" disabled={busy || limit} onClick={() => void add(product.id)} aria-label={`เพิ่ม ${product.name} ลงตะกร้า`}>
              {soldOut ? "สินค้าหมดชั่วคราว" : limit ? "ครบจำนวนที่มีแล้ว" : "เพิ่มลงตะกร้า"}<span aria-hidden="true">{soldOut || limit ? "—" : "+"}</span>
            </button>
            <p className="in-cart">{inCart > 0 ? `อยู่ในตะกร้า ${inCart} ชิ้น` : "\u00a0"}</p>
          </article>;
        })}</div>}
    </section>
    <section className="shop-banner"><div><span className="eyebrow">READY WHEN YOU ARE</span><h2>ของที่เลือกไว้ รออยู่ในตะกร้า</h2></div><Link href="/cart" className="button secondary">ดูตะกร้าของฉัน <span aria-hidden="true">↗</span></Link></section>
  </>;
}
