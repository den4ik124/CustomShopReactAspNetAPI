import React from "react";
import { Button, Container, Menu } from "semantic-ui-react";

export default function NavBar(){
    return(
        <Menu inverted pointing>
            <Container>
                <Menu.Item header content="Ships"/>
                <Menu.Item>
                    <Button positive content="Get ships list"/>
                </Menu.Item>
            </Container>
            <Container>
                <Menu.Item position="right">
                    Register
                </Menu.Item>
                <Menu.Item>
                    Login
                    </Menu.Item>
            </Container>
        </Menu>
    );
}