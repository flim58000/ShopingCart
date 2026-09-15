export interface Product {
  id: number;
  code: string;
  name: string;
  priceSatang: number;
  stockQuantity: number;
}

export interface CartItem {
  productId: number;
  productCode: string;
  productName: string;
  unitPriceSatang: number;
  stockQuantity: number;
  quantity: number;
  lineTotalSatang: number;
}

export interface Cart {
  id: string;
  status: "Open" | "CheckedOut";
  createdAt: string;
  items: CartItem[];
  totalSatang: number;
  totalQuantity: number;
}

export interface Order {
  id: number;
  cartId: string;
  totalSatang: number;
  createdAt: string;
  items: Omit<CartItem, "stockQuantity">[];
}
