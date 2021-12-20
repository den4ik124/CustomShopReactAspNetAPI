import { createContext, useContext } from "react";
import CommonStore from "./commonStore";
import DemoStore from "./demoStore";
import UserStore from "./userStore";

interface Store{
    commonStore : CommonStore
    userStore : UserStore
    demoStore : DemoStore
}

export const store: Store = {
    commonStore: new CommonStore(),
    userStore: new UserStore(),
    demoStore: new DemoStore()
}

export const StoreContext = createContext(store);

export function useStore(){
    return useContext(StoreContext);
}

