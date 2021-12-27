import { createContext, useContext } from "react";
import CommonStore from "./commonStore";
import DemoStore from "./demoStore";
import ProductStore from "./productStore";
import RoleStore from "./roleStore";
import UserStore from "./userStore";

interface Store{
    commonStore : CommonStore
    userStore : UserStore
    demoStore : DemoStore
    productStore: ProductStore
    roleStore: RoleStore
}

export const store: Store = {
    commonStore: new CommonStore(),
    userStore: new UserStore(),
    demoStore: new DemoStore(),
    productStore: new ProductStore(),
    roleStore: new RoleStore()
}

export const StoreContext = createContext(store);

export function useStore(){
    return useContext(StoreContext);
}

