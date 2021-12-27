import { makeAutoObservable, runInAction } from "mobx";
import agent from "../api/agent";
import { Role } from "../models/role";

export default class RoleStore{
    role: Role | null = null;
    roles: Role[] | null = null;

    constructor(){
        makeAutoObservable(this)
    }

    getProducts = async () => {
        try{
            const roles = await agent.Roles.list();
            console.log(roles);
            runInAction(()=>this.roles = roles);
        }catch (error){
            console.log(error);
        }
    }
}
