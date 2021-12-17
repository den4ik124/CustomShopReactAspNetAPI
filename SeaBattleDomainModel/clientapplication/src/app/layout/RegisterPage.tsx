import {  Form,Formik } from "formik";
import React from "react";
import { Button, ButtonGroup, ButtonOr, Container, Loader } from "semantic-ui-react";
import * as Yup from 'yup'
import MyTextInput from "../common/MyTextInput";

export default function RegisterPage(){
    const validationSchema = Yup.object({
        userName: Yup.string().required('The "user name" field is required!'),
        email: Yup.string().required('The "email" field is required!'),
        password: Yup.string().required('The "password" field is required!'),
        confirmPassword: Yup.string().required('You should confirm your password!')
    })

    function handleRegisterSubmit(){
        //TODO: change register logic
        console.log('Register submit')

    }

    function handleCancel(){
        //TODO: implement redirection logic
        console.log('Cancelation button')
    }

    return(
        <Container>
            <Loader/>
            <Formik 
                validationSchema={validationSchema}
                initialValues={()=>(null)} 
                onSubmit={handleRegisterSubmit}>
            {({handleSubmit}) => (
                <Form className="ui form" onSubmit={handleSubmit} autoComplete="off">
                    <MyTextInput name="userName" placeholder="User name"/>
                    <MyTextInput name="email" placeholder="Email"/>
                    <MyTextInput name="password" placeholder="Password"  type='password'/>
                    <MyTextInput name="confirmPassword" placeholder="Confirm password"  type='password'/>
                    <Button.Group fluid>
                        <Button positive color='black' type='submit'>Register</Button>
                        <ButtonOr/>
                        <Button color='grey' type='button' onClick={handleCancel}>Cancel</Button>
                    </Button.Group>
                </Form>
            )}
        </Formik>
        </Container>
        
    )
}