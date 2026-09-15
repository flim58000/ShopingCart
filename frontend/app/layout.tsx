import type { Metadata } from "next";
import { ShopProvider } from "../components/ShopProvider";
import { Header } from "../components/Header";
import "./globals.css";

export const metadata: Metadata = {
  title: "everyday — ของใช้สำหรับวันทำงาน",
  description: "เลือกสินค้า จัดการตะกร้า และทดลองชำระเงินได้ง่าย ๆ",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return <html lang="th"><body><ShopProvider>
    <Header />
    <main>{children}</main>
    <footer className="site-footer"><span className="footer-brand">everyday.</span><span>สิ่งเล็ก ๆ ที่ทำให้ทุกวันดีขึ้น</span><span>DEMO STORE · 2026</span></footer>
  </ShopProvider></body></html>;
}
