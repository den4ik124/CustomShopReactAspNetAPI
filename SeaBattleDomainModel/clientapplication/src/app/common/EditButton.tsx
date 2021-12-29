import React from "react";
import { Button, Icon, Popup, SemanticFLOATS } from "semantic-ui-react";

interface Props{
    onClick : () => void ;
    floated? : SemanticFLOATS;
}

export default function EditButton(props: Props){
    return(
        <Popup content='Edit' 
        mouseEnterDelay={500}
        mouseLeaveDelay={500}
        on='hover'
        trigger={
            <Button onClick={props.onClick} color="orange" floated={props.floated} >
                <Icon name="edit"/>
            </Button>
        } />
    )
}