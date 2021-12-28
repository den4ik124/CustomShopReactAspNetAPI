import { ErrorMessage, Formik, FormikErrors} from "formik";
import { observer } from "mobx-react-lite";
import React, { Fragment, useEffect, useState } from "react";
import { Item,  Button, Label, Container, Form } from "semantic-ui-react";
import agent from "../../api/agent";
import MyTextInput from "../../common/MyTextInput";
import { Role, RoleFormValues } from "../../models/role";
import { useStore } from "../../stores/store";
import LoadingComponent from "../components/LoadingComponents";

function RolesPage(){
    const {roleStore} = useStore();
    const [roles, setRoles] = useState<Role[]>([]);
    const [shouldUpdate, setUpdateList] = useState(false);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        agent.Roles.list().then(response => {
            console.log('List has been updated');
            setRoles(response);
            setLoading(false);
            setUpdateList(false);
        })
    }, [shouldUpdate])

    //console.log(roles);

     function handleRoleCreation (roleForm : RoleFormValues, setErrors: any){
        //console.log(roleForm) ;
        roleStore.createRole(roleForm)
        console.log('Role has been submitted') ;
        setUpdateList(true);
        //console.log(shouldUpdate)

     }

    function handleRemoveRole(roleId : string){
        //console.log(roleId);
        roleStore.removeRole(roleId);
        setUpdateList(true);
        //console.log(shouldUpdate)
    }

    if(loading) return <LoadingComponent inverted={false} content="Loading roles..."/>

    return(
        <Fragment>
            <Label ribbon  color="red" size="huge" content="Page is in design progress ..."/>
            <Container>
                <Formik 
                initialValues={{
                    roleName : "",
                    error: null
                }}
                onSubmit={ (initialValues, {setErrors}) => handleRoleCreation(initialValues, setErrors)}>
                    {({handleSubmit, errors}) => (
                        <Form className="ui form" onSubmit={handleSubmit} autoComplete="off">
                            <Form.Group width="equal">
                                <MyTextInput name="roleName" placeholder="Role name" type="text"/>
                                <Button  positive content="+ new role" type='submit'/>
                                <ErrorMessage 
                                    name="error"
                                    render={() => 
                                        <Label 
                                            basic 
                                            color="red"
                                            style ={{marginBottom: 10}}
                                            content={errors.error}
                                        />
                                    }
                                />
                            </Form.Group>
                        </Form>
                    )}
                </Formik>
            </Container>

            <Item.Group divided unstackable>
                {roles.map((role) => (
                    <Item key={role.name}>
                    <Item.Image size='tiny' src={`/sources/img/roles/${role.name}.png`} />
        
                    <Item.Content>
                        <Item.Header>
                            {role.name}
                            <Item.Extra>
                            <Button negative floated='right' onClick={() => handleRemoveRole(role.id)}>Remove</Button>
                            <Button color="orange" floated='right' >Edit</Button>
                        </Item.Extra>
                        </Item.Header>
                        <Item.Meta>Access description here</Item.Meta>
                        <Item.Extra>{role.description}</Item.Extra>
                    </Item.Content>
                    </Item>
                ))}
            </Item.Group>
        </Fragment>
    )
}

export default observer(RolesPage);