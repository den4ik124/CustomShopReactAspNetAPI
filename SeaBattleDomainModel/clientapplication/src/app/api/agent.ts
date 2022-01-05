import axios, { AxiosResponse } from "axios";
import { OrderItem } from "../models/orderItem";
import { Product, ProductCreationForm } from "../models/product";
import { Role, RoleFormValues } from "../models/role";
import { Ship } from "../models/ship";
import { User, UserFormValues } from "../models/user";
import { store } from "../stores/store";

const sleep = (delay: number) => {
    return new Promise((resolve) => {
        setTimeout(resolve, delay)
    })
}

axios.defaults.baseURL = "http://localhost:5000"

axios.interceptors.request.use(config =>{
    const token = store.commonStore.token;
    if(token) config.headers!.Authorization = `Bearer ${token}`;
    return config;
})

axios.interceptors.response.use(async response => {
    try {
        await sleep(500);
        return response;
    } catch (error) {
        console.log(error);
        return await Promise.reject(error);
    }
})

const responseBody = <T> (response: AxiosResponse<T>) => response.data;

const requests = {
    get: <T> (url: string) => axios.get<T>(url).then(responseBody),
    post:<T> (url: string, body: {}) => axios.post<T>(url, body).then(responseBody),
    put: <T>(url: string, body: {}) => axios.put<T>(url, body).then(responseBody),
    delete:<T> (url: string) => axios.delete<T>(url).then(responseBody),
}

const Ships = {
    list: () => requests.get<Ship[]>('/ships')
}
const Users = {
    list: () => requests.get<User[]>('/account/users'),
    addRoles: (userName: string, roles : string[]) => requests.put<void>(`/Roles/addRoles_${userName}`, roles),
    removeRole: (userName: string, role : string[]) => requests.put<void>(`/Roles/removeRole_${userName}`, role)
}

const Products ={
    list: () => requests.get<Product[]>('/shop'),
    add: (product : ProductCreationForm) => requests.post<void>('/Shop', product),
    remove: (id : string) => requests.delete<void>(`/Shop/${id}`),
    edit: (product : Product) => requests.put<void>(`/Shop/product_id${product.id}`, product)
}

const Order = {
    create: (order : OrderItem[]) => requests.post<void>('/Shop/createOrder', order),
}

const Account = {
    current: () => requests.get<User>('/account'),
    login: (user: UserFormValues) => requests.post<User>('/account/login', user),
    register: (user: UserFormValues) => requests.post<User>('/account/register', user)
}

const Roles ={
    list: () => requests.get<Role[]>('/roles'),
    add: (role : RoleFormValues) => requests.post<void>('/roles/AddRole', role),
    remove: (roleId : string) => requests.delete<void>(`/roles/delete/${roleId}`),
}

const agent ={
    Ships,
    Account,
    Users,
    Products,
    Roles,
    Order,
}

export default agent;