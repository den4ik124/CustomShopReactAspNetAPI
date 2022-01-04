import { makeAutoObservable } from "mobx";
import { Product } from "../models/product";
import { OrderItem } from "../models/orderItem";

export default class OrderItemStore{
    orderItem: OrderItem | null = null;
    orderItems: OrderItem[] = [];

    constructor(){
        makeAutoObservable(this)
    }


    createOrderItem = ( product : Product) => {
        try {
            this.orderItem = {
                id : product.id,
                product : product,
                productAmount : 1,
                isActive: true,
                totalCost : (product, count) => product.price * count 
            };
            var itemIndex = this.orderItems.findIndex((item) => (this.orderItem!.id === item.id));
            if(itemIndex > -1){
                this.orderItems[itemIndex].productAmount ++;
            }
            else{
                this.orderItems.push(this.orderItem);
            }
            //this.orderItem?.totalCost(product, count);            
        } catch (error) {
            console.log(error);
        }
    }

    getTotalCost = () => {
        var totalCost = 0;
        
        for (let index = 0; index < this.orderItems.length; index++) {
            if(this.orderItems[index].isActive){
                totalCost += this.orderItems[index].product.price * this.orderItems[index].productAmount;
            }
        }
        return totalCost;
    }

}
