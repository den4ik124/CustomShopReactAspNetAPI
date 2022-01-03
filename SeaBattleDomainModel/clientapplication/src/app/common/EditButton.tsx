import React from "react";
import { Button, Icon, Popup, SemanticFLOATS } from "semantic-ui-react";

interface Props{
    onClick : () => void ;
    floated? : SemanticFLOATS;
}

export default function EditButton(props: Props){
    return(

        <Button animated='fade' color="orange" floated={props.floated} onClick={props.onClick}>
            <Button.Content visible>
                <Icon name="edit"/>
            </Button.Content>
            <Button.Content hidden>Edit</Button.Content>
        </Button>

        // <Popup content='Edit' 
        // mouseEnterDelay={500}
        // mouseLeaveDelay={500}
        // on='hover'
        // trigger={
        //     <Button onClick={props.onClick} color="orange" floated={props.floated} >
        //         <Icon name="edit"/>
        //     </Button>
        // } />
    )
}