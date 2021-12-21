import { ErrorMessage, Formik, FormikErrors } from "formik";
import React from "react";
import { Button, Form, Label } from "semantic-ui-react";
import MyTextInput from "../common/MyTextInput";
import * as Yup from 'yup';
import { useStore } from "../stores/store";
import { observer } from "mobx-react-lite";
import { UserFormValues } from "../models/user";

function LoginPage(){
    const {userStore} = useStore();

    const validationSchema = Yup.object({
        password: Yup.string().required('The "password" field is required!'),
    })


    function handleLoginSubmit(values: UserFormValues, setErrors: any){
        if(values.emailProp === "")
            values.emailProp = null;
        else if(values.loginProp === "")
            values.loginProp = null;            

        console.log('Login submit')
        userStore.login(values)
                .catch(error => setErrors({error: 'Invalid email or password'}))

    }

    return(
        <Formik 
            validationSchema={validationSchema}
            initialValues ={{
                loginProp: '',
                emailProp: '',
                password: '',
                error: null
            }}
            onSubmit={(initialValues, {setErrors}) => handleLoginSubmit(initialValues, setErrors)}
        >
        {({handleSubmit, isSubmitting, errors}) => (
            <Form className="ui form" onSubmit={handleSubmit} autoComplete="off" size="large">
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
                <Form.Group widths="equal">
                    <MyTextInput name="loginProp" placeholder="User name"/>
                    <Label size="big" content="or"/>
                    <MyTextInput name="emailProp" placeholder="Email"/>
                </Form.Group>
                <MyTextInput name = 'password' placeholder='Password' type='password'/>
                <Button loading={isSubmitting} fluid positive type='submit'>Login</Button>
            </Form>
        )}
    </Formik>
    )
}

export default observer(LoginPage);