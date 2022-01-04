import { observer } from "mobx-react-lite"
import React, { Fragment, useEffect, useState } from "react"
import { Button, Header, Item, Label } from "semantic-ui-react"
import agent from "../../api/agent";
import DeleteButton from "../../common/DeleteButton";
import EditButton from "../../common/EditButton";
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
                        
                        {loggedUser!.roles.includes("Admin") ? (
                            <>
                                <DeleteButton name={user.loginProp}  floated="right" onClick={() => null}/>
                                <EditButton floated='right' onClick={() => null}/>
                            </>
                            ) : null}
                        </Item.Extra>
                    </Item.Content>
                </Item>
            ))}
        </Item.Group>
    </Fragment>
    )
}

export default observer(UsersPage)
