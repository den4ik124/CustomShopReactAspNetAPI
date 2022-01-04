import { runInAction } from "mobx";
import { observer } from "mobx-react-lite";
import React, { SyntheticEvent, useState } from "react";
import { Button, Container, Divider, Form, Grid, Header, Icon, Input, Item, Label, Radio, Rail, Segment, Statistic, Sticky } from "semantic-ui-react";
import { setFlagsFromString } from "v8";
import { OrderItem } from "../../models/orderItem";
import { useStore } from "../../stores/store";
import './gridCustomStyles.css';



 function ProductCart(){
    const {orderItemStore} = useStore()
    const {orderItemStore: {orderItems}} = useStore()
    const [disabled, setDisable] = useState(false);
    const [target, setTarget] = useState('');

const style = {
    outline: "none"
}

    if(orderItems.length < 1 ){
        return (
            <Header>You did not select any product from list</Header>
        )
    }

function decreaseCount(orderItem : OrderItem){
    runInAction(() => orderItem.productAmount--);
    if(orderItem.productAmount < 1){
       orderItem.productAmount =1;
    }
console.log('Counter has been decreased');
}

function increaseCount(orderItem : OrderItem){
    runInAction(() => orderItem.productAmount++);
console.log('Counter has been increased');
}

function handleRemoveItemFromCart(e:  React.MouseEvent<HTMLElement, MouseEvent>,
     item : OrderItem){

    runInAction(() => item.isActive = false) ;
    console.log(e.currentTarget.tagName);
    setTarget(e.currentTarget.tagName);
    setDisable(true);
}

function handleFinalRemoveItemFromCart(item : OrderItem){
    const index = orderItemStore.orderItems.indexOf(item);
    if (index > -1) {
        orderItemStore.orderItems.splice(index, 1);
    }
}

function handleRestoreItem(e: React.MouseEvent<HTMLElement, MouseEvent> , item : OrderItem){
    runInAction(() => item.isActive = true) ;
    setTarget(e.currentTarget.tagName);
    setDisable(false);
}
    return(
        <Grid columns={2}>
            <Grid.Column>
                <Header> Your products</Header>
                <Item.Group>
                    {orderItems.map((item) => (
                        <>
                            <Item key={item.id} >
                                <>
                                {item.isActive ? null : (
                                    renderRestoreRemoveButtons(item)
                                )}
                                <Segment floated="left" style={{margin : "0px"}} name={item.id} disabled={disabled}>
                                <Label circular style={{border: "none", margin: '0px'}} attached="top left" basic onRemove={(e) => handleRemoveItemFromCart(e, item)}/>
                                <Grid className=".no-grid-magrin-top" relaxed='very' style={{marginTop: "0px !important"}} columns={5}>
                                    <Grid.Column width={4}>
                                        <Item.Image 
                                            size='tiny' 
                                            src={`/sources/img/products/${item.product.title}.png`}
                                        />
                                    </Grid.Column>
                                    <Grid.Column width={5}>
                                        <Item.Header>{item.product.title}</Item.Header>
                                        <Item.Meta>{item.product.description} Description</Item.Meta>
                                    </Grid.Column>
                                    <Grid.Column width={3}>
                                    <Form >
                                            <Button disabled={disabled} icon='arrow up' size="mini" onClick={() => increaseCount(item)}/>
                                            <Input
                                            transparent
                                            disabled={disabled}
                                                style ={{minWidth: "50px"}}
                                                name='productCount'
                                                placeholder='1'
                                                value={item.productAmount}
                                                
                                            />
                                            <Button disabled={disabled} icon='arrow down' size="mini" onClick={() => decreaseCount(item)}/>
                                    </Form>
                                    </Grid.Column>
                                    <Grid.Column width={3}>
                                        <Item.Content className='price' content={`${item.product.price} UAH`}/>
                                    </Grid.Column>
                                </Grid>
                                </Segment>
                            </>
                            </Item>
                        </>
                    ))}
                    
                </Item.Group>

                <Rail position='right'>
                    <Sticky offset={50} >
                    <Segment color="grey">
                        <Header as='h3'>Сумма заказа</Header>
                        <Statistic size="small">
                            <Statistic.Value content={`${orderItemStore.getTotalCost()} UAH`}/>
                            <Statistic.Label content='Total cost'/>
                        </Statistic>
                        <Divider/>
                        <Button positive content='Оформить заказ'/>
                    </Segment>
                    </Sticky>
                </Rail>
            </Grid.Column>
        </Grid>
    )

     function renderRestoreRemoveButtons(item: OrderItem): React.ReactNode {
         return <Button.Group style={{outline: 'none'}} compact vertical>
             <Button 
                style={style}
                compact
                basic
                disabled={!disabled} 
                color="green"
                icon 
                onClick={(e) => handleRestoreItem(e, item)}
             >
                 Restore
                 <Icon name='check' />
             </Button>
             <Button 
                style={style} 
                compact 
                basic 
                size="tiny" 
                disabled={!disabled} 
                color="red" 
                icon 
                onClick={() => handleFinalRemoveItemFromCart(item)}
             >
                 <Icon name='trash' />
             </Button>
         </Button.Group>;
     }
}

export default observer(ProductCart)
