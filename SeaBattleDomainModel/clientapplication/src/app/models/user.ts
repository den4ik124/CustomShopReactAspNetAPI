export interface User{
    loginProp: string;
    emailProp: string;
    token: string;
}

export interface UserFormValues{
    emailProp?: string | null;
    password: string;
    loginProp?: string | null;
}