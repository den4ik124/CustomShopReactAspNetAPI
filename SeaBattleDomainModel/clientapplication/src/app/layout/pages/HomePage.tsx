import { observer } from "mobx-react-lite";
import React from "react";
import { Link } from "react-router-dom";
import { Button, Container, Header, Label } from "semantic-ui-react";
import { useStore } from "../../stores/store";

function HomePage(){
    const {userStore} = useStore();

    return(
        <Container style={{marginTop: '7em' }}>
            {userStore.isLoggedIn ? (
                <Container>
                    <Header content={'Welcome ' + userStore.user!.loginProp + ' !'}/>
                    {console.log(userStore.isLoggedIn)}
                    <Button as={Link} to='/Products' color="blue" size='huge' inverted>
                        Buy some products    
                    </Button>    
                </Container>
            ) : (
                <Container>
                    <Header content='Home page' size="huge"/>
                    {console.log(userStore.isLoggedIn)}
                    <Button as={Link} to='/Login' color="blue" size='huge' inverted>
                        Login !    
                    </Button>    
                    <Button as={Link} to='/Register' color="blue" size='huge' inverted>
                        Register !    
                    </Button>    
                </Container>     
            )}
        </Container>
    )
}

export default observer(HomePage);