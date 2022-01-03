import { observer } from "mobx-react-lite";
import React, { Fragment, useEffect, useState } from "react";
import { Item, Button, Label,  Container, Header, Card, Icon, Image, Grid} from "semantic-ui-react";
import agent from "../../api/agent";
import DeleteButton from "../../common/DeleteButton";
import EditButton from "../../common/EditButton";
import { Product } from "../../models/product";
import { useStore } from "../../stores/store";
import LoadingComponent from "../components/LoadingComponents";
import CreateNewProduct from "./Modals/CreateNewProduct";

function ProductsPage(){
    const [products, setProducts] = useState<Product[]>([]);
    const [loading, setLoading] = useState(true);
    const {userStore} = useStore()
    const {productStore} = useStore()
    const [shouldUpdate, setUpdateList] = useState(false);


    if(userStore.isLoggedIn){
        var user =  userStore.user;
    }

    useEffect(() => {
        agent.Products.list().then(response => {
            setProducts(response);
          setLoading(false);
          setUpdateList(false)
        })
    }, [shouldUpdate])

function renderControllButtons(product : Product){

    if(user!.roles.includes('Manager') || user!.roles.includes('Admin')){
        return(
            <>
                <DeleteButton floated="right" onClick={() => handleRemove(product.id)}/>
                <EditButton floated='right' onClick={() => null}/>
            </>
        )
    }
}

    console.log(products);

    if(loading) return <LoadingComponent content="Loading products..."/>

    function handleRemove(id: string){
        console.log('Product has been removed...');
        productStore.removeProduct(id);
        setUpdateList(true);
    }

return(
    <Fragment>
        <Label ribbon color="red" size="huge" content="Page is in design progress ..."/>
        {user!.roles.includes('Manager') || user!.roles.includes('Admin') ? (
            <>
                <CreateNewProduct trigger={<Button fluid positive content="Add new product"
                style={{marginBottom: "50px"}} />}/>
            </>
        ) : null}

        <Grid columns={3}>
        {products.map((product) => (
            <Grid.Column>
                <Card >
                    <Image 
                    fluid
                    rounded
                    wrapped
                    
                        // style={{marginRight: "50px", 
                        //         width: "200px", 
                        //         height: "200px"}}
                         style={{marginRight: "50px"}} 

                        size='medium' 
                        src={`/sources/img/products/${product.title}.png`}
                    />
                    
                    <Card.Content textAlign="left">
                        <Card.Header>
                            {product.title}
                            {renderControllButtons(product)}

                        </Card.Header>
                        <Card.Meta>
                            <span className='date'>Joined in 2015</span>
                        </Card.Meta>
                        <Card.Description>
                            {product.description}
                            Lorem ipsum dolor sit amet consectetur adipisicing elit. Quam, distinctio sed ipsa rem unde minus enim quasi id ipsam iusto nisi eum sapiente. Sint sapiente rem voluptatibus eos nobis sequi.
                        </Card.Description>
                    </Card.Content>
                    <Card.Content extra>
                        <Header 
                            fluid
                            text
                            textAlign="center"
                            color="green"
                            content={product.price + ' UAH'}
                        />
                        <Button 
                        fluid
                            positive 
                            position="right" 
                            content='Buy now!'
                        />
                    </Card.Content>
                </Card>
            </Grid.Column>
        ))}
        </Grid>
        <Item.Group divided unstackable>
            {products.map((product) => (

            <>
            </>

                // <Item key={product.title}>
                //     {/* https://konti.ua/download/superkontik-logo.png */}
                // {/* <Item.Image style={{marginRight: "50px"}} size='medium' src={`https://konti.ua/download/superkontik-logo.png`} /> */}
                // <Item.Image style={{marginRight: "50px"}} size='medium' src={`/sources/img/products/${product.title}.png`} />
                // <Item.Content>
                //     <Item.Header>
                //         <Header content={product.title}/>
                //     </Item.Header>
                //     <Item.Extra>
                //         <Container>
                //             {renderControllButtons(product)}
                //             <Label size="massive">{product.price} UAH</Label>
                //             <Button positive> Buy now!</Button>
                //         </Container>
                //         <Container>{product.description}</Container>
                //     </Item.Extra>
                // </Item.Content>
                // </Item>
            ))}
        </Item.Group>
    </Fragment>
)
}

export default observer(ProductsPage);