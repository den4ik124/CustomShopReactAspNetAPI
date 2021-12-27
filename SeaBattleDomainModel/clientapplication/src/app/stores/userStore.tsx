import { makeAutoObservable, runInAction } from "mobx";
import { history } from "../..";
import agent from "../api/agent";
import { User, UserFormValues } from "../models/user";
import { store } from "./store";

export default class UserStore{
    user: User | null = null;
    users: User[] | null = null;

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
            store.commonStore.setToken(user.token)
            runInAction(() => this.user = user);
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
            runInAction(()=>{
                this.user = user;
                //this.user.roles.push('Admin'); //TODO : help code to check roles impact
            });
        }catch (error){
            console.log(error);
        }
    }

    getUsers = async () => {
        try{
            const users = await agent.Users.list();
            console.log(users);
            runInAction(()=>this.users = users);
        }catch (error){
            console.log(error);
        }
    }
}
