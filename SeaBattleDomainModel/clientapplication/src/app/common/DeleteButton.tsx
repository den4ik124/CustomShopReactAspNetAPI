import React, { SyntheticEvent, useState } from "react";
import { Button, Icon, Label, Popup, Reveal, SemanticFLOATS } from "semantic-ui-react";

interface Props{
    name: string;
    onClick : () => void ;
    floated? : SemanticFLOATS;
}

export default function DeleteButton(props: Props){
    const [loading, setLoading] = useState(false);
    const[target, setTarget] = useState('');

function handleDelete(e: SyntheticEvent<HTMLButtonElement>){
    console.log(e.currentTarget.name);
    setTarget(e.currentTarget.name) 
    props.onClick();
    setLoading(true);
}

    return(
        <>
            <Button 
                compact
                loading={loading} 
                name={props.name} 
                animated='fade' 
                color="red" 
                floated={props.floated} 
                onClick={(e)=> handleDelete(e)}
            >
                <Button.Content visible>
                    <Icon name="trash"/>
                </Button.Content>
                <Button.Content hidden>Remove</Button.Content>
            </Button>
        </>
    )
}