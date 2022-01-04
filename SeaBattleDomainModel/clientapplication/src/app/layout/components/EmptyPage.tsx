import React from "react";
import { Link } from "react-router-dom";
import { Button, Header, Icon, Segment } from "semantic-ui-react";

interface Props{
    message: string;
}

export default function EmptyPage({message} : Props){
    return(
        <Segment placeholder>
            <Header icon>
            <Icon name="file alternate"/>
                {message}
            </Header>
            <Button as={Link} to='/Products' primary>Return to the shop</Button>
        </Segment>
    )
}