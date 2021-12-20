import { action, makeAutoObservable, makeObservable, observable } from "mobx";

export default class DemoStore{
    message = "Hello from DemoStore";

    constructor(){
        makeAutoObservable(this)
            // makeObservable(this,{
            //     message: observable,
            //     setMessage: action
            // })
    }

    setMessage = () => {
        this.message = this.message + "!"; 
        console.log( '>> ' + this.message)
    }
}