import {  ErrorMessage, Formik } from "formik";
import React from "react";
import { Button,  Form, ButtonOr, Container, Loader, Label } from "semantic-ui-react";
import * as Yup from 'yup'
import { history } from "../..";
import MyTextInput from "../common/MyTextInput";
import { UserFormValues } from "../models/user";
import { useStore } from "../stores/store";

export default function RegisterPage(){

    const {userStore} = useStore();

    const validationSchema = Yup.object({
        loginProp: Yup.string().required('The "user name" field is required!'),
        emailProp: Yup.string().email().required('The "email" field is required!'),
        password: Yup.string().required('The "password" field is required!'),
        confirmPassword: Yup.string().oneOf([Yup.ref('password'), null],"Passwords must match") .required('You should confirm your password!')
    })

    function handleRegisterSubmit(values: UserFormValues, setErrors: any){
        //TODO: change register logic
        console.log('Register submit')

        console.log('Password :' + values.password);
        console.log('Confirm password :' + values.confirmPassword);
        if(values.confirmPassword !== values.password){
            setErrors('Password and Confirm password not matched')
        } else {
            userStore.register(values)
                .catch(error => setErrors({error: 'Invalid email or password'}))
        }

    }

    function handleCancel(){
        //TODO: implement redirection logic
        console.log('Cancelation button')
        history.push('/');
    }

    return(
        <Container>
            <Loader/>
            <Formik 
                validationSchema={validationSchema}
                initialValues ={{
                    loginProp: '',
                    emailProp: '',
                    password: '',
                    confirmPassword: '',
                    error: null
                }}
                onSubmit={(initialValues, {setErrors}) => handleRegisterSubmit(initialValues, setErrors)}>
            {({handleSubmit, isSubmitting, errors}) => (
                <Form className="ui form" onSubmit={handleSubmit} autoComplete="off">
                    <ErrorMessage 
                    name="error"
                    render={() => 
                        <Label 
                            basic 
                            color="red"
                            style ={{marginBottom: 10}}
                            content={errors.error}
                        />
                    }
                />
                    <MyTextInput name="loginProp" placeholder="User name"/>
                    <MyTextInput name="emailProp" placeholder="Email"/>
                    <MyTextInput name="password" placeholder="Password"  type='password'/>
                    <MyTextInput name="confirmPassword" placeholder="Confirm password"  type='password'/>
                    <Button.Group fluid>
                        <Button  loading={isSubmitting} positive color='black' type='submit'>Register</Button>
                        <ButtonOr/>
                        <Button color='grey' type='button' onClick={handleCancel}>Cancel</Button>
                    </Button.Group>
                </Form>
            )}
        </Formik>
        </Container>
        
    )
}