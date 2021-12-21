import { observer } from "mobx-react-lite";
import React, { Fragment } from "react";
import { Link, NavLink } from "react-router-dom";
import { Button, Container, Label, Menu, Segment } from "semantic-ui-react";
import { useStore } from "../stores/store";

function NavBar(){
    const {userStore: {user, logout}} = useStore();
    
    return(
        //#region old NavBar
        // <Menu inverted pointing>
        //     <Container>
        //         <Menu.Item header>
        //             <Button as={Link} to={'/ships'} content="Ships"/>
        //         </Menu.Item>
        //     </Container>
        //     <Container>
        //         {userStore.isLoggedIn ? (
            //             <Menu.Item position="right" > 
            //                 <Button content="Logout" onClick={() => userStore.logout}/>
            //             </Menu.Item>    
            //         ) : (
                //             <Fragment>
                //                 <Menu.Item position="right" >
        //                     <Button as={Link} to={'/Register'} content="Register"/>
        //                 </Menu.Item>
        //                 <Menu.Item>
        //                     <Button as={Link} to={'/Login'} content="Login"/>
        //                 </Menu.Item>
        //             </Fragment>
        //         )}
        //     </Container>
        // </Menu>
        //#endregion 
        
        <>
        <Menu inverted pointing >
            <Container>
                <Menu.Item as={NavLink} to='/' name='Home' />
                {/* <Menu.Item as={NavLink} to='/ships' name='Ships' exact/> */}
                {/* <Menu.Item text={'Welcome' + userStore.user!.loginProp!}/> */}
                {/* TODO: How to add user name inside */}


                {user != null ? (
                    <>
                        <Menu.Item position="right">
                            <Label content={user.loginProp} color='green' size='large'/>    
                        </Menu.Item> 
                        <Menu.Item as={NavLink} to='/' onClick={logout} name="Logout" />
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