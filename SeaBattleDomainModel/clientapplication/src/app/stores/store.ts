import { createContext, useContext } from "react";
import DemoStore from "./demoStore";
import UserStore from "./userStore";

interface Store{
    userStore : UserStore
    demoStore : DemoStore
}

export const store: Store = {
    userStore: new UserStore(),
    demoStore: new DemoStore()
}

export const StoreContext = createContext(store);

export function useStore(){
    return useContext(StoreContext);
}

