import { observer } from "mobx-react-lite";
import React, { Fragment, useEffect, useState } from "react";
import { Item, Image, Button, Label} from "semantic-ui-react";
import agent from "../../api/agent";
import { Role } from "../../models/role";
import LoadingComponent from "../components/LoadingComponents";

function RolesPage(){

    const [roles, setRoles] = useState<Role[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        agent.Roles.list().then(response => {
            setRoles(response);
          setLoading(false);
        })
    }, [])

    console.log(roles);

    if(loading) return <LoadingComponent content="Loading roles..."/>

return(
    <Fragment>
        <Label ribbon  color="red" size="huge" content="Page is in design progress ..."/>
        <Button fluid positive content="+ new role"/>
        <Item.Group divided unstackable>
            {roles.map((role) => (
                <Item key={role.name}>
                <Item.Image size='tiny' src={`/sources/img/roles/${role.name}.png`} />
    
                <Item.Content>
                    <Item.Header>
                        {role.name}
                        <Item.Extra>
                        <Button negative floated='right'>Remove</Button>
                        <Button color="orange" floated='right'>Edit</Button>
                    </Item.Extra>
                    </Item.Header>
                    <Item.Meta>Full access</Item.Meta>
                    <Item.Description>
                        <Image src={`/sources/img/roles/${role.name}.png`} size="medium"/>
                    </Item.Description>
                    <Item.Extra>{role.description}</Item.Extra>
                </Item.Content>
                </Item>
            ))}
        </Item.Group>
    </Fragment>
)
}

export default observer(RolesPage);