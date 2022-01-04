import React, { SyntheticEvent, useState } from "react";
import { Button, Icon, SemanticFLOATS } from "semantic-ui-react";

interface Props{
    name: string
    onClick : () => void ;
    floated? : SemanticFLOATS;
}

export default function BuyProductButton(props: Props){
const [isDisabled, setDisable] = useState(false);
const [target, setTarget] = useState('');

function handleClick(e : SyntheticEvent<HTMLButtonElement>) {
    setTarget(e.currentTarget.name),
    props.onClick,
    setDisable(true)
}
    return(
        <>
        <Button 
        fluid
        name = {props.name}
            onClick={(e) => handleClick(e)}
            animated='vertical' positive disabled={isDisabled}>
            <Button.Content  hidden>Done</Button.Content>
            <Button.Content  visible>
                <Icon name='shop' />
                Buy now!
            </Button.Content>
        </Button>
        </>
    )
}


