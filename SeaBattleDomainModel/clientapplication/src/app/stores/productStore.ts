import { makeAutoObservable, runInAction } from "mobx";
import { history } from "../..";
import agent from "../api/agent";
import { Product } from "../models/product";
import { User, UserFormValues } from "../models/user";
import { store } from "./store";

export default class ProductStore{
    product: Product | null = null;
    products: Product[] | null = null;

    constructor(){
        makeAutoObservable(this)
    }

    getProducts = async () => {
        try{
            const products = await agent.Products.list();
            console.log(products);
            runInAction(()=>this.products = products);
        }catch (error){
            console.log(error);
        }
    }
}
