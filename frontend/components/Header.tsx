"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useShop } from "./ShopProvider";

export function Header() {
  const pathname = usePathname();
  const { cart, error, notice, busy, refresh } = useShop();
  return <>
    <div className="top-strip">ของใช้ดี ๆ เพื่อทุกวันของคุณ <span>EVERYDAY ESSENTIALS</span></div>
    <header className="site-header">
      <Link href="/" className="brand" aria-label="everyday หน้าสินค้า"><span className="brand-mark">e.</span>everyday<span className="brand-dot">®</span></Link>
      <nav aria-label="เมนูหลัก">
        <Link href="/" aria-current={pathname === "/" ? "page" : undefined}>สินค้าทั้งหมด</Link>
        <Link href="/cart" className="cart-nav" aria-current={pathname === "/cart" ? "page" : undefined}>
          ตะกร้าของฉัน <span className="count">{cart?.totalQuantity ?? 0}</span>
        </Link>
      </nav>
    </header>
    <div className="messages">
      {error && <div className="alert" role="alert"><span>{error}</span><button onClick={() => void refresh()} disabled={busy}>ลองใหม่</button></div>}
      <div className="notice" role="status" aria-live="polite">{notice || "\u00a0"}</div>
    </div>
  </>;
}
