import { observer } from "mobx-react-lite";
import React, { useEffect, useState } from "react";
import { Button, Divider, Grid, Header, Item, Rail, Segment, Statistic, Sticky } from "semantic-ui-react";
import { useStore } from "../../stores/store";
import EmptyPage from "../components/EmptyPage";
import OrderListItem from "../components/OrderListItem";
import './gridCustomStyles.css';



 function ProductCart(){
    const {orderItemStore} = useStore()
    const {orderItemStore: {orderItems}} = useStore()
    const [disabled, setDisabled] = useState(false);

    
    var totalCost = orderItemStore.getTotalCost();
    useEffect(() => {
        if(totalCost <= 0 ){
            setDisabled(true)
        }
        else(
            setDisabled(false)
        )
    }, [totalCost])

    if(orderItems.length < 1 ){
        return (
            <EmptyPage message="You did not select any product from list"/>
        )
    }

    return(
        <Grid columns={2}>
            <Grid.Column>
                <Header> Your products</Header>
                <Item.Group>
                    {orderItems.map((item) => (
                        <OrderListItem key={item.id} item={item}/>
                    ))}
                    
                </Item.Group>

                <Rail position='right'>
                    <Sticky offset={50} >
                    <Segment textAlign="center" inverted color="olive">
                        <Header as='h3'>Сумма заказа</Header>
                        <Statistic size="small">
                            <Statistic.Value content={`${orderItemStore.getTotalCost()} UAH`}/>
                            <Statistic.Label content='Total cost'/>
                        </Statistic>
                        <Divider/>
                        <Button positive disabled={disabled} content='Оформить заказ'/>
                    </Segment>
                    </Sticky>
                </Rail>
            </Grid.Column>
        </Grid>
    )

    
}

export default observer(ProductCart)
