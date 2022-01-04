import { Product } from "./product";

export interface OrderItem{
    id: string;
    product: Product;
    productAmount: number;
    isActive : boolean;
    totalCost: (product : Product, productAmount: number) => number;
}
    