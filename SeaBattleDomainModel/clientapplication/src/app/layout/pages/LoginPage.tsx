import { ErrorMessage, Formik, FormikErrors } from "formik";
import React from "react";
import { Button, Container, Divider, Form, Grid, Icon, Label, Modal, Segment } from "semantic-ui-react";
import MyTextInput from "../../common/MyTextInput";
import * as Yup from 'yup';
import { useStore } from "../../stores/store";
import { observer } from "mobx-react-lite";
import { UserFormValues } from "../../models/user";
import './loginForm.css';


interface Props{
    trigger: React.ReactNode
}

function LoginPage(props : Props){
  const [open, setOpen] = React.useState(false)
  const {orderItemStore} = useStore()

    const {userStore} = useStore();

    const validationSchema = Yup.object({
        password: Yup.string().required('The "password" field is required!'),
    })


    function handleLoginSubmit(values: UserFormValues, setErrors: any){
        if(values.emailProp === "")
            values.emailProp = null;
        else if(values.loginProp === "")
            values.loginProp = null;            
        orderItemStore.orderItems = [];
        console.log('Login submit')
        userStore.login(values)
                .catch(error => setErrors({error: 'Invalid email or password'}))

    }

    function handleGoogleSingIn(){
            console.log('Sign in with google')
    }


    return(
        <Modal
            onClose={() => setOpen(false)}
            onOpen={() => setOpen(true)}
            open={open}
            trigger={props.trigger}
        >
      <Modal.Header className="" content="Login" />
      <Modal.Content></Modal.Content>
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
                    <Segment>
                        <Grid columns={2}>
                            <Grid.Column>
                                <MyTextInput name="loginProp" placeholder="User name"/>
                            </Grid.Column>
                            <Grid.Column>
                                <MyTextInput name="emailProp" placeholder="Email"/>
                            </Grid.Column>
                        </Grid>
                        <Divider vertical>Or</Divider>    
                    </Segment>
                    <Container textAlign="center">
                        <MyTextInput style={{maxWidth : '300px', marginBottom: '10px'}} name = 'password' placeholder='Password' type='password'/>
                        <Button loading={isSubmitting} fluid positive type='submit'>Login</Button>
                    </Container>


                </Form>
            )}
        </Formik>
        <Divider horizontal content={'OR SIGN IN WITH'}/>
        <Container textAlign="center">
                <Button className="googleAuthButton" fluid color='google plus' onClick={handleGoogleSingIn}>
                    <Icon name='google plus official' />
                    Google
                </Button>
                <Button className="facebook" fluid color='facebook'>
                    <Icon name='facebook official' />
                    Facebook
                </Button>
        </Container>
    </Modal>
    )
}

export default observer(LoginPage);