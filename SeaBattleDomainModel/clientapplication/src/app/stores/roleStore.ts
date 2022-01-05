import { makeAutoObservable, runInAction } from "mobx";
import agent from "../api/agent";
import { Role, RoleFormValues } from "../models/role";

export default class RoleStore{
    role: Role | null = null;
    roles: Role[] = [];

    constructor(){
        makeAutoObservable(this)
    }

    getRoles = async () => {
        try{
            const roles = await agent.Roles.list();
            console.log(roles);
            runInAction(()=>this.roles = roles);
        }catch (error){
            console.log(error);
        }
    }

    createRole = async (roleForm : RoleFormValues) => {
        try{
            await agent.Roles.add(roleForm);
            console.log(roleForm);
        }catch (error){
            console.log(error);
        }
    }

    removeRole = async (roleId : string) => {
        try{
            await agent.Roles.remove(roleId);
            console.log(roleId);
        }catch (error){
            console.log(error);
        }
    }

}
