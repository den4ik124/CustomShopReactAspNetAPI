import { observer } from "mobx-react-lite";
import React, { useEffect } from "react";
import {  NavLink } from "react-router-dom";
import { Container, Dropdown, Icon, Label, Menu } from "semantic-ui-react";
import { useStore } from "../../stores/store";

function NavBar(){
    const {userStore: {user, logout}} = useStore();
    const {orderItemStore} = useStore();

    const orderItemsCount = orderItemStore.orderItems.length;

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

    useEffect(()=> {
        console.log('Product was added into cart')
    }, [orderItemsCount])

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
                        {orderItemsCount > 0 ? (
                        <>
                        <Menu.Item 
                            name = 'cart'
                            as={NavLink} to='/ProductCart'>
                            <Icon name="shopping cart"/>
                                {orderItemsCount}
                        </Menu.Item>
                        </>) : null}
      
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