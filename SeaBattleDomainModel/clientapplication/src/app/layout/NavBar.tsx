import React, { Fragment } from "react";
import { Link } from "react-router-dom";
import { Button, Container, Menu, Segment } from "semantic-ui-react";
import { useStore } from "../stores/store";

export default function NavBar(){
    const {userStore} = useStore();

    return(
        <Menu inverted pointing>
            <Container>
                <Menu.Item header>
                    <Button as={Link} to={'/ships'} content="Ships"/>
                </Menu.Item>
            </Container>
            <Container>
                {userStore.isLoggedIn ? (
                    <Menu.Item>
                        <Button content="Logout" onClick={() => userStore.logout}/>
                    </Menu.Item>    
                ) : (
                    <Fragment>
                        <Menu.Item position="right" >
                            <Button as={Link} to={'/Register'} content="Register"/>
                        </Menu.Item>
                        <Menu.Item>
                            <Button as={Link} to={'/Login'} content="Login"/>
                        </Menu.Item>
                    </Fragment>
                )}
            </Container>
        </Menu>
    );
}