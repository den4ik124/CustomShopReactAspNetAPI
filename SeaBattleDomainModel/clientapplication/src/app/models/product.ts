export interface Product{
    id: string;
    title: string;
    price: number;
    description: string;
}

export interface ProductCreationForm{
    id: string;
    title: string;
    price: number;
    description?: string;
}
