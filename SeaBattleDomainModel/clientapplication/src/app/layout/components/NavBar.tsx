import { observer } from "mobx-react-lite";
import React from "react";
import {  NavLink } from "react-router-dom";
import { Container, Dropdown, DropdownItem, DropdownMenu, Label, Menu } from "semantic-ui-react";
import { useStore } from "../../stores/store";

function NavBar(){
    const {userStore: {user, logout}} = useStore();
    
    return(
        <>
        <Menu inverted pointing >
            <Container>
                <Menu.Item as={NavLink} to='/' exact name='Home' />
                <Menu.Item as={NavLink} to='/Products' name='Products' content="Products" />
                {user != null && user.roles.includes('Admin') ? (
                    <Dropdown pointing text="Admins" className="link item" >
                        <Dropdown.Menu>
                            <Dropdown.Item as={NavLink} to='/admin/roles' name='Roles' content="Roles (admins only)" />
                            <Dropdown.Item as={NavLink} to='/admin/users' name='Users' content="Users  (admins only)" />
                        </Dropdown.Menu>
                    </Dropdown>
                ) : (
                    <></>
                )}

                {user != null ? (
                    <>
                        <Menu.Item exact position="right">
                            <Label content={user.loginProp} color='green' size='large'/>    
                        </Menu.Item> 
                        <Menu.Item exact as={NavLink} to='/' onClick={logout} name="Logout" />
                    </>
                ) : (
                        <></>
                )}
            </Container>
        </Menu>
        </>

    );
}

export default observer(NavBar);