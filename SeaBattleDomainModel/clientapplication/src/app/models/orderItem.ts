import { Product } from "./product";

export interface OrderItem{
    id: string;
    product: Product;
    productAmount: number;
    totalCost: (product : Product, productAmount: number) => number;
}
    