import { observer } from "mobx-react-lite";
import React from "react";
import {  NavLink } from "react-router-dom";
import { Container, Dropdown, Label, Menu } from "semantic-ui-react";
import { useStore } from "../../stores/store";

function NavBar(){
    const {userStore: {user, logout}} = useStore();
    
    return(
        <>
        <Menu inverted pointing >
            <Container>
                <Menu.Item as={NavLink} to='/' exact name='Home' />
    
                    { user != null ? (
                    <>
                        <Menu.Item as={NavLink} to='/Products' name='Products' content="Products" />
                        {user != null && user.roles.includes('Admin') ? (
                            <Dropdown pointing text="Admins" className="link item" >
                                <Dropdown.Menu>
                                    <Dropdown.Item as={NavLink} to='/admin/roles' name='Roles' content="Roles (admins only)" />
                                    <Dropdown.Item as={NavLink} to='/admin/users' name='Users' content="Users  (admins only)" />
                                </Dropdown.Menu>
                            </Dropdown>
                        ) : null}
                    </>
                    ) : null}

                    
                {user != null ? (
                    <>
                        <Menu.Item  position="right">
                            <Label content={user.loginProp} color='green' size='large'/>    
                        </Menu.Item> 
                        <Menu.Item exact as={NavLink} to='/' onClick={logout} name="Logout" />
                    </>
                ) : null}
            </Container>
        </Menu>
        </>

    );
}

export default observer(NavBar);