export interface User{
    loginProp: string;
    emailProp: string;
    token: string;
}

export interface UserFormValues{
    emailProp?: string | null;
    password: string;
    confirmPassword?: string;
    loginProp?: string | null;
}