import { observer } from "mobx-react-lite";
import React from "react";
import {  NavLink } from "react-router-dom";
import { Container, Dropdown, Icon, Label, Menu } from "semantic-ui-react";
import { User } from "../../models/user";
import { useStore } from "../../stores/store";

function NavBar(){
    const {userStore: {user, logout}} = useStore();
    
    function renderUserName(){
        var login = `Welcome back ${user!.loginProp}`;
        if(user!.roles.includes('Admin')){
            return <Label content={login} color='green' size='large'/>    
        }
        else if(user!.roles.includes('Manager')){
            return <Label content={login} color='yellow' size='large'/>    
        }
        else{
            return login;
        }
    }

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
                        <Menu.Item  position="right" content={renderUserName()}/>
                        <Menu.Item 
                            name = 'cart'
                            content={'Cart (products count)'}
                            as={NavLink} to='/ProductCart' />
                        <Menu.Item exact as={NavLink} to='/' onClick={logout} content={
                            <Icon size="large" name="log out"/>
                        } name="Logout" />
                    </>
                ) : null}
            </Container>
        </Menu>
        </>

    );
}

export default observer(NavBar);