import React from "react";
import { Button, Icon, Label, Popup, Reveal, SemanticFLOATS } from "semantic-ui-react";

interface Props{
    onClick : () => void ;
    floated? : SemanticFLOATS;
}

export default function DeleteButton(props: Props){
    return(
        <>
            <Button animated='fade' color="red" floated={props.floated} onClick={props.onClick}>
                <Button.Content visible>
                    <Icon name="trash"/>
                </Button.Content>
                <Button.Content hidden>Remove</Button.Content>
            </Button>
        </>
    )
}