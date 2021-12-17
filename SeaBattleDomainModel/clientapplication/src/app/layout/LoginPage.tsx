import { Formik } from "formik";
import React from "react";
import { Button,  ButtonOr,  Form, Label } from "semantic-ui-react";
import MyTextInput from "../common/MyTextInput";
import * as Yup from 'yup';

export default function LoginPage(){
    const validationSchema = Yup.object({
        password: Yup.string().required('The "password" field is required!'),
    })

    function handleLoginSubmit(){
        //TODO: change login logic
        console.log('Login submit')
        
        //TODO: implement redirection logic

    }

    return(
        <Formik 
        validationSchema={validationSchema}
        initialValues={{
            userName: '',
            email: '',
            password: ''
        }}
        onSubmit={handleLoginSubmit}
        >
        {({handleSubmit}) => (
            <Form onSubmit={handleSubmit} autoComplete="off" size="large">
                <Form.Group widths="equal">
                    <MyTextInput name="userName" placeholder="User name"/>
                    <Label size="big" content="or"/>
                    <MyTextInput name="email" placeholder="Email"/>
                </Form.Group>
                <MyTextInput name = 'password' placeholder='Password' type='password'/>
                <Button fluid positive type='submit'>Login</Button>
            </Form>
        )}
    </Formik>
    )
}