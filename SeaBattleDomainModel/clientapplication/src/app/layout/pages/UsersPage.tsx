import { observer } from "mobx-react-lite"
import React, { Fragment, useEffect, useState } from "react"
import { Button, Header, Item, Label } from "semantic-ui-react"
import agent from "../../api/agent";
import { User } from "../../models/user";
import { useStore } from "../../stores/store";
import LoadingComponent from "../components/LoadingComponents";

function UsersPage(){

    const [users, setUsers] = useState<User[]>([]);
    const [loading, setLoading] = useState(true);
    const {userStore} = useStore()

    if(userStore.isLoggedIn){
        var loggedUser =  userStore.user;
    }

    useEffect(() => {
        agent.Users.list().then(response => {
            setUsers(response);
          setLoading(false);
        })
    }, [])

    console.log(users);

    if(loading) return <LoadingComponent content="Loading users..."/>

    return(
        <Fragment>
        <Label ribbon  color="red" size="huge" content="Page is in design progress ..."/>
        <Item.Group divided unstackable>
            {users.map((user) => (
                <Item key={user.loginProp}>
                    <Item.Image size='tiny' src={`/sources/img/userLogo/defaultUser.png`} />
                    <Item.Content>
                        <Item.Header>
                            {user.loginProp}
                        </Item.Header>
                       
                        <Item.Extra>
                            <>
                                <Label content={user.emailProp} />
                                {user.roles.map((role) => (
                                    <Label key={role} color="teal" content= {`${role} `}/>
                                ))}
                            </>
                        </Item.Extra>
                        
                        {loggedUser!.roles.includes("Admin") ? (
                            <Item.Extra>
                                <Button negative floated='right'>Remove</Button>
                                <Button color="orange" floated='right'>Edit</Button>
                            </Item.Extra>
                            ) : null}
                    </Item.Content>
                </Item>
            ))}
        </Item.Group>
    </Fragment>
    )
}

export default observer(UsersPage)
