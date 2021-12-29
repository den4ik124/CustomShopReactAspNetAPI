import React from "react";
import { Button, Icon, Label, Popup, Reveal, SemanticFLOATS } from "semantic-ui-react";

interface Props{
    onClick : () => void ;
    floated? : SemanticFLOATS;
}

export default function DeleteButton(props: Props){
    return(
        <>
            <Popup content='Delete' 
                mouseEnterDelay={500}
                mouseLeaveDelay={500}
                on='hover'
                trigger={
                    <Button onClick={props.onClick} color="red" floated={props.floated} >
                        <Icon name="trash"/>
                    </Button>
            } />
        
        </>
    )
}