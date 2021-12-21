import { makeAutoObservable, runInAction } from "mobx";
import { history } from "../..";
import agent from "../api/agent";
import { User, UserFormValues } from "../models/user";
import { store } from "./store";

export default class UserStore{
    user: User | null = null;

    constructor(){
        makeAutoObservable(this)
    }

    get isLoggedIn(){
        return !!this.user;
    }

    register = async (creds: UserFormValues) => {
        try{
            const user = await agent.Account.register(creds);
            runInAction(() => this.user = user)
            store.commonStore.setToken(user.token)
            history.push('/');
        }catch(error){
            console.log(error);
        }
    }

    login = async (creds: UserFormValues) => {
        try{
            const user = await agent.Account.login(creds);
            // console.log('User from API');
            // console.log(user);
            store.commonStore.setToken(user.token)
            runInAction(() => this.user = user);
            // console.log('User from userStore');
            // console.log(this.user!.emailProp);
            // console.log(this.user!.token);
            history.push('/');
        } catch(error){
            throw error;
        }
    }

    logout = () => {
        store.commonStore.setToken(null);
        window.localStorage.removeItem('jwt');
        this.user = null;
        history.push('/')
    };

    getUser = async () => {
        try{
            const user = await agent.Account.current();
            console.log(user);
            runInAction(()=>this.user = user);
        }catch (error){
            console.log(error);
        }
    }
}
