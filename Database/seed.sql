-- ใช้ ON CONFLICT เพื่อให้เปิดแอปซ้ำได้โดยไม่เติมสต็อกที่ขายไปแล้วกลับมา
INSERT INTO Products (Code, Name, PriceSatang, StockQuantity) VALUES
    ('P001', 'เมาส์',     19900, 10),
    ('P002', 'คีย์บอร์ด', 59000, 5),
    ('P003', 'หูฟัง',     89050, 2),
    ('P004', 'จอภาพ',    399000, 0)
ON CONFLICT (Code) DO NOTHING;
