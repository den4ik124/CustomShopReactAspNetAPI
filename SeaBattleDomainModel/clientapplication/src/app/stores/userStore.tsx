import { makeAutoObservable, makeObservable, runInAction } from "mobx";
import agent from "../api/agent";
import { User, UserFormValues } from "../models/user";

export default class UserStore{
    user: User | null = null;
    
    constructor(){
        makeAutoObservable(this)
    }

    get isLoggedIn(){
        return !!this.user;
    }

    login = async (creds: UserFormValues) => {
        try{
            const user = await agent.Account.login(creds);
            runInAction(() => this.user = user);

        } catch(error){
            throw error;
        }
    }

    logout = () => {
        this.user = null;
    }
}
