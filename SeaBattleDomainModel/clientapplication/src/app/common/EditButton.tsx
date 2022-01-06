import React from "react";
import { Button, Icon, Popup, SemanticFLOATS } from "semantic-ui-react";

interface Props{
    onClick? : () => void ;
    floated? : SemanticFLOATS;
}

export default function EditButton(props: Props){
    return(
        <Button compact animated='fade' color="orange" floated={props.floated} onClick={props.onClick}>
            <Button.Content visible>
                <Icon name="edit"/>
            </Button.Content>
            <Button.Content hidden>Edit</Button.Content>
        </Button>
    )
}