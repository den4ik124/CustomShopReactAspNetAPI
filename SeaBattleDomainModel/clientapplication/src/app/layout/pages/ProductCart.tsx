import { runInAction } from "mobx";
import { observer } from "mobx-react-lite";
import React from "react";
import { Button, Divider, Form, Grid, Header, Input, Item, Rail, Statistic, Sticky } from "semantic-ui-react";
import { OrderItem } from "../../models/orderItem";
import { useStore } from "../../stores/store";


 function ProductCart(){
    const {orderItemStore} = useStore()
    const {orderItemStore: {orderItems}} = useStore()


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

    return(
        <Grid columns={2}>
            <Grid.Column>
                <Header> Your products</Header>
                <Item.Group divided>
                    {orderItems.map((item) => (
                        <>
                            <Item key={item.id}>
                                <Grid columns={4}>
                                    <Grid.Column>
                                        <Item.Image 
                                            size='tiny' 
                                            src={`/sources/img/products/${item.product.title}.png`}
                                        />
                                    </Grid.Column>
                                    <Grid.Column>
                                        <Item.Header>{item.product.title}</Item.Header>
                                        <Item.Meta>{item.product.description} Description</Item.Meta>
                                    </Grid.Column>
                                    <Grid.Column>
                                    <Form >
                                        <Form.Group>
                                            <Button icon='minus' size="mini" onClick={() => decreaseCount(item)}/>
                                            <Input
                                                style ={{minWidth: "50px"}}
                                                name='productCount'
                                                placeholder='1'
                                                value={item.productAmount}
                                            />
                                            <Button icon='plus' size="mini" onClick={() => increaseCount(item)}/>
                                        </Form.Group>
                                    </Form>
                                    </Grid.Column>
                                    <Grid.Column>
                                        <Item.Content className='price' content={`${item.product.price} UAH`}/>
                                    </Grid.Column>
                                </Grid>
    
                            </Item>
                        </>
                    ))}
                    
                </Item.Group>

                <Rail position='right'>
                    <Sticky offset={50} >
                        <Header as='h3'>Сумма заказа</Header>
                        <Statistic size="small">
                            <Statistic.Value content={`${orderItemStore.getTotalCost()} UAH`}/>
                            <Statistic.Label content='Total cost'/>
                        </Statistic>
                        <Divider/>
                        <Button positive content='Оформить заказ'/>
                    </Sticky>
                </Rail>
            </Grid.Column>
        </Grid>
    )
}

export default observer(ProductCart)
