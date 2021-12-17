import React from "react";
import { Link } from "react-router-dom";
import { Button, Container, Menu } from "semantic-ui-react";

export default function NavBar(){
    return(
        <Menu inverted pointing>
            <Container>
                <Menu.Item header content="Ships">
                    <Button as={Link} to={'/ships'} content="Ships"/>
                </Menu.Item>
                {/* <Menu.Item>
                    <Button positive content="Get ships list"/>
                </Menu.Item> */}
            </Container>
            <Container>
                <Menu.Item  position="right">
                    <Button as={Link} to={'/Register'} content="Register"/>
                </Menu.Item>
                <Menu.Item>
                    <Button as={Link} to={'/Login'} content="Login"/>
                </Menu.Item>
            </Container>
        </Menu>
    );
}